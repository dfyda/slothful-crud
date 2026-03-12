using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using System.Reflection;
using System.Reflection.Emit;

namespace SlothfulCrud.Tests.Unit.Configurations
{
    public class SlothConfigurationBuilderTests
    {
        private const string DynamicModuleName = "Main";
        private const string GeneratedEntityTypeName = "GeneratedEntity";
        private const string GeneratedEntityConfigurationTypeName = "GeneratedEntityConfiguration";
        private const string GeneratedEntityConfigurationDuplicateTypeName = "GeneratedEntityConfigurationDuplicate";
        private const string DisplayNameGetterName = $"get_{nameof(ISlothfulEntity.DisplayName)}";
        private const string GeneratedDisplayName = "Generated";
        private static readonly ConstructorInfo ObjectParameterlessConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;

        [Fact]
        public void ApplyConfigurationsFromAssembly_ShouldApplyConfiguration_ForConfiguredEntity()
        {
            // Arrange
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act
            configurationBuilder.ApplyConfigurationsFromAssembly(typeof(ConfiguredEntityConfiguration).Assembly);
            var builder = configurationBuilder.GetBuilder<ConfiguredEntity>();
            var configuration = builder.Build();

            // Assert
            Assert.Equal(nameof(ConfiguredEntity.Code), configuration.Entity.KeyProperty);
            Assert.Equal(typeof(long), configuration.Entity.KeyPropertyType);
            Assert.Equal(nameof(ConfiguredEntity.DisplayName), configuration.Entity.SortProperty);
            Assert.Equal("Patch", configuration.Entity.UpdateMethod);
            Assert.True(configuration.Get.IsAuthorizationEnable);
            Assert.Contains("Policy.Get", configuration.Get.PolicyNames);
        }

        [Fact]
        public void GetBuilder_ShouldReturnDefaultBuilder_WhenEntityHasNoConfiguration()
        {
            // Arrange
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act
            var builder = configurationBuilder.GetBuilder<NotConfiguredEntity>();
            var configuration = builder.Build();

            // Assert
            Assert.Equal("Id", configuration.Entity.KeyProperty);
            Assert.Equal(typeof(Guid), configuration.Entity.KeyPropertyType);
            Assert.Equal("Name", configuration.Entity.SortProperty);
            Assert.Equal("Update", configuration.Entity.UpdateMethod);
        }

        [Fact]
        public void ApplyConfiguration_ShouldApplyProvidedConfiguration_ForSingleEntity()
        {
            // Arrange
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act
            configurationBuilder.ApplyConfiguration(new ConfiguredEntityConfiguration());
            var configuration = configurationBuilder.GetBuilder<ConfiguredEntity>().Build();

            // Assert
            Assert.Equal(nameof(ConfiguredEntity.Code), configuration.Entity.KeyProperty);
            Assert.Equal(typeof(long), configuration.Entity.KeyPropertyType);
        }

        [Fact]
        public void ApplyConfigurationsFromAssembly_ShouldApplyMultipleConfigurations_ForDifferentEntities()
        {
            // Arrange
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act
            configurationBuilder.ApplyConfigurationsFromAssembly(typeof(ConfiguredEntityConfiguration).Assembly);
            var first = configurationBuilder.GetBuilder<ConfiguredEntity>().Build();
            var second = configurationBuilder.GetBuilder<SecondConfiguredEntity>().Build();

            // Assert
            Assert.Equal(nameof(ConfiguredEntity.Code), first.Entity.KeyProperty);
            Assert.Equal(nameof(SecondConfiguredEntity.CustomId), second.Entity.KeyProperty);
            Assert.Equal(typeof(Guid), second.Entity.KeyPropertyType);
            Assert.Equal("Rename", second.Entity.UpdateMethod);
            Assert.False(second.Delete.IsEnable);
        }

        [Fact]
        public void ApplyConfiguration_ShouldThrowConfigurationException_WhenConfigurationForEntityAlreadyExists()
        {
            // Arrange
            var configurationBuilder = new SlothConfigurationBuilder();
            configurationBuilder.ApplyConfiguration(new ConfiguredEntityConfiguration());

            // Act + Assert
            var exception = Assert.Throws<ConfigurationException>(() =>
                configurationBuilder.ApplyConfiguration(new ConfiguredEntityConfiguration()));

            Assert.Contains(nameof(ConfiguredEntity), exception.Message);
        }

        [Fact]
        public void ApplyConfigurationsFromAssembly_ShouldThrowConfigurationException_WhenConfigurationHasNoPublicParameterlessConstructor()
        {
            // Arrange
            var assembly = BuildDynamicConfigurationAssembly(includeDuplicateConfiguration: false, missingParameterlessConstructor: true);
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act + Assert
            var exception = Assert.Throws<ConfigurationException>(() => configurationBuilder.ApplyConfigurationsFromAssembly(assembly));
            Assert.Contains("public parameterless constructor", exception.Message);
        }

        [Fact]
        public void ApplyConfigurationsFromAssembly_ShouldThrowConfigurationException_WhenSameEntityIsConfiguredMoreThanOnce()
        {
            // Arrange
            var assembly = BuildDynamicConfigurationAssembly(includeDuplicateConfiguration: true, missingParameterlessConstructor: false);
            var configurationBuilder = new SlothConfigurationBuilder();

            // Act + Assert
            var exception = Assert.Throws<TargetInvocationException>(() => configurationBuilder.ApplyConfigurationsFromAssembly(assembly));
            var rootException = exception.InnerException as ConfigurationException
                                ?? (exception.InnerException as TargetInvocationException)?.InnerException as ConfigurationException;
            Assert.NotNull(rootException);
            Assert.Contains("already registered", rootException.Message);
        }

        private class ConfiguredEntity : ISlothfulEntity
        {
            public long Code { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class SecondConfiguredEntity : ISlothfulEntity
        {
            public Guid CustomId { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class NotConfiguredEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class ConfiguredEntityConfiguration : ISlothEntityConfiguration<ConfiguredEntity>
        {
            public void Configure(SlothEntityBuilder<ConfiguredEntity> builder)
            {
                builder
                    .SetKeyProperty(x => x.Code)
                    .SetSortProperty(x => x.DisplayName)
                    .SetUpdateMethodName("Patch");

                builder.GetEndpoint.RequireAuthorization("Policy.Get");
            }
        }

        private class SecondConfiguredEntityConfiguration : ISlothEntityConfiguration<SecondConfiguredEntity>
        {
            public void Configure(SlothEntityBuilder<SecondConfiguredEntity> builder)
            {
                builder
                    .SetKeyProperty(x => x.CustomId)
                    .SetUpdateMethodName("Rename");

                builder.DeleteEndpoint.HasEndpoint(false);
            }
        }

        private static Assembly BuildDynamicConfigurationAssembly(
            bool includeDuplicateConfiguration,
            bool missingParameterlessConstructor)
        {
            var assemblyName = new AssemblyName($"DynamicConfigAssembly_{Guid.NewGuid():N}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(DynamicModuleName);

            var entityType = DefineEntityType(moduleBuilder, GeneratedEntityTypeName);
            DefineConfigurationType(moduleBuilder, entityType, GeneratedEntityConfigurationTypeName, hasParameterlessConstructor: !missingParameterlessConstructor);

            if (includeDuplicateConfiguration)
            {
                DefineConfigurationType(moduleBuilder, entityType, GeneratedEntityConfigurationDuplicateTypeName, hasParameterlessConstructor: true);
            }

            return assemblyBuilder;
        }

        private static Type DefineEntityType(ModuleBuilder moduleBuilder, string typeName)
        {
            ArgumentNullException.ThrowIfNull(moduleBuilder);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("Type name cannot be null or whitespace.", nameof(typeName));
            }

            var typeBuilder = moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class);

            typeBuilder.AddInterfaceImplementation(typeof(ISlothfulEntity));

            DefinePublicConstructor(typeBuilder, Type.EmptyTypes);

            var getDisplayNameMethod = typeBuilder.DefineMethod(
                DisplayNameGetterName,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(string),
                Type.EmptyTypes);
            var displayNameIl = getDisplayNameMethod.GetILGenerator();
            displayNameIl.Emit(OpCodes.Ldstr, GeneratedDisplayName);
            displayNameIl.Emit(OpCodes.Ret);

            var propertyBuilder = typeBuilder.DefineProperty(
                nameof(ISlothfulEntity.DisplayName),
                PropertyAttributes.None,
                typeof(string),
                null);
            propertyBuilder.SetGetMethod(getDisplayNameMethod);

            var interfaceGetter = typeof(ISlothfulEntity).GetProperty(nameof(ISlothfulEntity.DisplayName))!.GetGetMethod()!;
            typeBuilder.DefineMethodOverride(getDisplayNameMethod, interfaceGetter);

            return typeBuilder.CreateType()!;
        }

        private static void DefineConfigurationType(
            ModuleBuilder moduleBuilder,
            Type entityType,
            string typeName,
            bool hasParameterlessConstructor)
        {
            ArgumentNullException.ThrowIfNull(moduleBuilder);
            ArgumentNullException.ThrowIfNull(entityType);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("Type name cannot be null or whitespace.", nameof(typeName));
            }

            var interfaceType = typeof(ISlothEntityConfiguration<>).MakeGenericType(entityType);
            var builderType = typeof(SlothEntityBuilder<>).MakeGenericType(entityType);

            var typeBuilder = moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(interfaceType);

            if (hasParameterlessConstructor)
            {
                DefinePublicConstructor(typeBuilder, Type.EmptyTypes);
            }
            else
            {
                DefinePublicConstructor(typeBuilder, [typeof(string)]);
            }

            var configureMethod = typeBuilder.DefineMethod(
                nameof(ISlothEntityConfiguration<ISlothfulEntity>.Configure),
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(void),
                [builderType]);
            var configureIl = configureMethod.GetILGenerator();
            configureIl.Emit(OpCodes.Ret);

            var interfaceMethod = interfaceType.GetMethod(nameof(ISlothEntityConfiguration<ISlothfulEntity>.Configure))!;
            typeBuilder.DefineMethodOverride(configureMethod, interfaceMethod);

            _ = typeBuilder.CreateType();
        }

        private static void DefinePublicConstructor(TypeBuilder typeBuilder, Type[] parameterTypes)
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes);

            EmitBaseConstructorBody(ctorBuilder);
        }

        private static void EmitBaseConstructorBody(ConstructorBuilder ctorBuilder)
        {
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, ObjectParameterlessConstructor);
            ctorIl.Emit(OpCodes.Ret);
        }
    }
}
