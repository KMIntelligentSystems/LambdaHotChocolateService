using System;
using System.Collections.Generic;
using System.Text;

namespace HotChocolateService
{
    public class Query
    {
        MessageFrom from = new MessageFrom { Id = "1", DisplayName = "test" };
        public Message GetMessage() => new Message { Content = "test", SentAt = new DateTime(), MessageFrom = new MessageFrom { Id = "1", DisplayName = "test" } };

        public string Hello() => "world";

        public string GetDisplayName() => from.DisplayName;
        public MessageFrom GetMessageFrom() => from;
    }
}
