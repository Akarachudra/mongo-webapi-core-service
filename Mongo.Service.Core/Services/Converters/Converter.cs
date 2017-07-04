using System.Collections.Generic;
using System.Linq;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services.Converters
{
    public abstract class Converter<TApi, TEntity> : IConverter<TApi, TEntity> where TEntity : IBaseEntity where TApi : IApiBase
    {
        public abstract TApi GetApiFromStorable(TEntity source);
        public abstract TEntity GetStorableFromApi(TApi source);

        public TEntity[] GetStorableFromApi(IEnumerable<TApi> source)
        {
            return source.Select(GetStorableFromApi).ToArray();
        }

        public TApi[] GetApiFromStorable(IEnumerable<TEntity> source)
        {
            return source.Select(GetApiFromStorable).ToArray();
        }
    }
}