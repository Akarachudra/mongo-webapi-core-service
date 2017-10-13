using System.Collections.Generic;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services.Mapping
{
    public interface IMapper<TApi, TEntity> where TEntity : IBaseEntity where TApi : IApiBase
    {
        TApi GetApiFromEntity(TEntity source);
        TApi[] GetApiFromEntity(IEnumerable<TEntity> source);
        TEntity GetEntityFromApi(TApi source);
        TEntity[] GetEntityFromApi(IEnumerable<TApi> source);
    }
}