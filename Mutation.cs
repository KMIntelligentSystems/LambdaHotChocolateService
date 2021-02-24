using HotChocolate;
using HotChocolate.Types;
using System;

namespace HotChocolateService
{
    public class Mutation
    {
        //private readonly IBookService _bookService;

        public Mutation(/*IBookService bookService*/)
        {
           // _bookService = bookService;
        }
        public Message AddMessage(MessageInput input)
        {
            var message = new Message()
            {
                Content = input.content,
                SentAt = input.sentAt,
                MessageFrom = input.messageFrom
            };
            return message;
        }

    }

    public class MessageInput
    {
        public string content { get; set; }
        public DateTime sentAt { get; set; }
        public MessageFrom messageFrom { get; set; }
    }
}
