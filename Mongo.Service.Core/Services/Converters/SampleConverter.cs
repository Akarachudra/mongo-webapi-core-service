using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Services.Converters
{
    public class SampleConverter : Converter<ApiSample, SampleEntity>
    {
        public override ApiSample GetApiFromStorable(SampleEntity source)
        {
            return new ApiSample
            {
                Id = source.Id,
                SomeData = source.SomeData
            };
        }

        public override SampleEntity GetStorableFromApi(ApiSample source)
        {
            return new SampleEntity
            {
                Id = source.Id,
                SomeData = source.SomeData
            };
        }
    }
}