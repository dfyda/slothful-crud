using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;
using System.Collections.Concurrent;

namespace SlothfulCrud.Tests.Unit.Types
{
    public class DynamicTypeTests
    {
        private const string ParentEntityDtoTypeName = "ParentEntityDto";
        private const string ParentEntityDetailsDtoTypeName = "ParentEntityDetailsDto";
        private const string ParentEntityFlatDtoTypeName = "ParentEntityFlatDto";
        private const string ParentEntityFlatWithNullDtoTypeName = "ParentEntityFlatWithNullDto";
        private const string ParentEntityExposeAllDtoTypeName = "ParentEntityExposeAllDto";
        private const string ParentEntityConcurrentDtoTypeName = "ParentEntityConcurrentDto";
        private const string ParentEntityPagedDtoTypeName = "ParentEntityPagedDto";
        private const string BrowseMethodName = "Browse";
        private const int ParallelIterations = 20;

        [Fact]
        public void NewDynamicTypeDto_ShouldUseBaseEntityDtoForNestedProperties_WhenExposeAllIsFalse()
        {
            // Act
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityDtoTypeName, false);

            // Assert
            Assert.Equal(typeof(BaseEntityDto), dtoType.GetProperty(nameof(ParentEntity.Child))?.PropertyType);
        }

        [Fact]
        public void NewDynamicTypeDto_ShouldKeepNestedType_WhenExposeAllIsTrue()
        {
            // Act
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityDetailsDtoTypeName, true);

            // Assert
            Assert.Equal(typeof(ChildEntity), dtoType.GetProperty(nameof(ParentEntity.Child))?.PropertyType);
        }

        [Fact]
        public void MapToDto_ShouldFlattenNestedEntityToBaseEntityDto_WhenExposeAllIsFalse()
        {
            // Arrange
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityFlatDtoTypeName, false);
            var item = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Child = new ChildEntity
                {
                    CustomId = 123L,
                    Name = "Child"
                }
            };

            // Act
            dynamic result = DynamicType.MapToDto(item, typeof(ParentEntity), dtoType, false, nameof(ChildEntity.CustomId));

            // Assert
            var nestedDto = Assert.IsType<BaseEntityDto>(result.Child);
            Assert.Equal(123L, nestedDto.Id);
            Assert.Equal("Child", nestedDto.DisplayName);
        }

        [Fact]
        public void MapToDto_ShouldKeepNestedEntityNull_WhenExposeAllIsFalseAndNestedIsNull()
        {
            // Arrange
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityFlatWithNullDtoTypeName, false);
            var item = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Child = null
            };

            // Act
            dynamic result = DynamicType.MapToDto(item, typeof(ParentEntity), dtoType, false, nameof(ChildEntity.CustomId));

            // Assert
            Assert.Null(result.Child);
        }

        [Fact]
        public void MapToDto_ShouldKeepNestedEntity_WhenExposeAllIsTrue()
        {
            // Arrange
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityExposeAllDtoTypeName, true);
            var item = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Child = new ChildEntity
                {
                    CustomId = 999L,
                    Name = "Child"
                }
            };

            // Act
            dynamic result = DynamicType.MapToDto(item, typeof(ParentEntity), dtoType, true, nameof(ChildEntity.CustomId));

            // Assert
            var nested = Assert.IsType<ChildEntity>(result.Child);
            Assert.Equal(999L, nested.CustomId);
            Assert.Equal("Child", nested.Name);
        }

        [Fact]
        public void NewDynamicType_ShouldReturnSameType_WhenCalledMultipleTimesWithSameSignature()
        {
            // Arrange
            var properties = new[] { new TypeProperty("Name", typeof(string)) };

            // Act
            var first = DynamicType.NewDynamicType(properties, typeof(ParentEntity), BrowseMethodName);
            var second = DynamicType.NewDynamicType(properties, typeof(ParentEntity), BrowseMethodName);

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void NewDynamicTypeDto_ShouldNotThrow_WhenCalledConcurrentlyWithSameTypeName()
        {
            // Arrange
            var exceptions = new ConcurrentBag<Exception>();

            // Act
            Parallel.For(0, ParallelIterations, _ =>
            {
                try
                {
                    DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityConcurrentDtoTypeName, false);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            });

            // Assert
            Assert.Empty(exceptions);
        }

        [Fact]
        public void MapToPagedResultsDto_ShouldMapNestedPropertiesToBaseEntityDto_WhenExposeAllIsFalse()
        {
            // Arrange
            var dtoType = DynamicType.NewDynamicTypeDto(typeof(ParentEntity), ParentEntityPagedDtoTypeName, false);
            var data = new List<ParentEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Parent1",
                    Child = new ChildEntity { CustomId = 10, Name = "Child1" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Parent2",
                    Child = new ChildEntity { CustomId = 20, Name = "Child2" }
                }
            };
            var paged = new PagedResults<ParentEntity>(0, 2, 10, data);

            // Act
            dynamic result = DynamicType.MapToPagedResultsDto(
                paged,
                typeof(ParentEntity),
                dtoType,
                false,
                nameof(ChildEntity.CustomId));

            // Assert
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Data.Count);
            var nestedDto = Assert.IsType<BaseEntityDto>(result.Data[0].Child);
            Assert.Equal(10L, nestedDto.Id);
            Assert.Equal("Child1", nestedDto.DisplayName);
        }

        [Fact]
        public void MapToPagedBaseEntityDto_ShouldMapIdAndDisplayName_FromConfiguredKeyProperty()
        {
            // Arrange
            var items = new List<BaseListEntity>
            {
                new() { CustomId = 7, Name = "A" },
                new() { CustomId = 8, Name = "B" }
            };
            var paged = new PagedResults<BaseListEntity>(0, 2, 10, items);

            // Act
            var result = DynamicType.MapToPagedBaseEntityDto(paged, nameof(BaseListEntity.CustomId));

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(7, result.Data[0].Id);
            Assert.Equal("A", result.Data[0].DisplayName);
            Assert.Equal(8, result.Data[1].Id);
            Assert.Equal("B", result.Data[1].DisplayName);
        }

        public class ParentEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public ChildEntity Child { get; init; }
        }

        public class ChildEntity
        {
            public long CustomId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string DisplayName => Name;
        }

        public class BaseListEntity
        {
            public int CustomId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string DisplayName => Name;
        }
    }
}
