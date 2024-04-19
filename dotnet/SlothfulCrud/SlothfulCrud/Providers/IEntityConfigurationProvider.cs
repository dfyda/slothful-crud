﻿using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    public interface IEntityConfigurationProvider
    {
        void Register(Type type, EntityConfiguration configuration);
        EntityConfiguration GetConfiguration(Type type);
    }   
}