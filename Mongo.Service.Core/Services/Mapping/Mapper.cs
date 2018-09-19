using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services.Mapping
{
    public class Mapper<TApi, TEntity> : IMapper<TApi, TEntity>
        where TEntity : IBaseEntity
        where TApi : IApiBase
    {
        public Mapper()
        {
            this.ConfigureMappers();
        }

        protected IMapper ApiToEntityMapper { get; set; }

        protected IMapper EntityToApiMapper { get; set; }

        public TApi GetApiFromEntity(TEntity source)
        {
            return this.EntityToApiMapper.Map<TApi>(source);
        }

        public TEntity GetEntityFromApi(TApi source)
        {
            return this.ApiToEntityMapper.Map<TEntity>(source);
        }

        public TEntity[] GetEntityFromApi(IEnumerable<TApi> source)
        {
            return source.Select(this.GetEntityFromApi).ToArray();
        }

        public TApi[] GetApiFromEntity(IEnumerable<TEntity> source)
        {
            return source.Select(this.GetApiFromEntity).ToArray();
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

        private void ConfigureMappers()
        {
            this.ApiToEntityMapper = this.ConfigureApiToEntityMapper();
            this.EntityToApiMapper = this.ConfigureEntityToApiMapper();
        }
    }
}