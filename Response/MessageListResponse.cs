using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing list of messages
    /// </summary>
    public class MessageListResponse : BaseResponse
    {
        // FIXME: Messages shouldn't actually be strings, set up "Message" object
        public string[]? messageList { get; set; }
        public long? totalMessage { get; set; }
        public string[]? operatorMessageList { get; set; }
        public long? totalOperatorMessage { get; set; }

        public MessageListResponse()
        {
            messageList = new string[0];
            totalMessage = 0;
            operatorMessageList = new string[0];
            totalOperatorMessage = 0;
        }
    }
}
