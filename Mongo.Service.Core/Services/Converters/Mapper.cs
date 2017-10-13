using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services.Converters
{
    public class Mapper<TApi, TEntity> : IMapper<TApi, TEntity> where TEntity : IBaseEntity where TApi : IApiBase
    {
        public Mapper()
        {
            ConfigureMappers();
        }

        public TApi GetApiFromEntity(TEntity source)
        {
            return EntityToApiMapper.Map<TApi>(source);
        }

        public TEntity GetEntityFromApi(TApi source)
        {
            return ApiToEntityMapper.Map<TEntity>(source);
        }

        public TEntity[] GetEntityFromApi(IEnumerable<TApi> source)
        {
            return source.Select(GetEntityFromApi).ToArray();
        }

        public TApi[] GetApiFromEntity(IEnumerable<TEntity> source)
        {
            return source.Select(GetApiFromEntity).ToArray();
        }

        protected virtual IMapper ConfigureApiToEntityMapper()
        {
            var toEntityMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TApi, TEntity>());
            return toEntityMapperConfig.CreateMapper();
        }

        protected virtual IMapper ConfigureEntityToApiMapper()
        {
            var toApiMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TEntity, TApi>());
            return toApiMapperConfig.CreateMapper();
        }

        protected IMapper ApiToEntityMapper { get; set; }
        protected IMapper EntityToApiMapper { get; set; }

        private void ConfigureMappers()
        {
            ApiToEntityMapper = ConfigureApiToEntityMapper();
            EntityToApiMapper = ConfigureEntityToApiMapper();
        }
    }
}