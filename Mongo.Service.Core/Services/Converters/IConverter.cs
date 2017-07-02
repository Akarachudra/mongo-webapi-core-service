using System.Collections.Generic;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Types.Base;

namespace Mongo.Service.Core.Services.Converters
{
    public interface IConverter<TApi, TEntity> where TEntity : IBaseEntity where TApi : IApiBase
    {
        TApi GetApiFromStorable(TEntity source);
        TApi[] GetApiFromStorable(IEnumerable<TEntity> source);
        TEntity GetStorableFromApi(TApi source);
        TEntity[] GetStorableFromApi(IEnumerable<TApi> source);
    }
}