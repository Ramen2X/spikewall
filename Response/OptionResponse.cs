using spikewall.Object;

namespace spikewall.Response
{
    public class UserResultResponse : BaseResponse
    {
        public UserResult optionUserResult { get; set; }

        public UserResultResponse()
        {
            optionUserResult = new UserResult();
        }
    }
}
