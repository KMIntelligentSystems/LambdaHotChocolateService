using System;

namespace HotChocolateService
{
    public class Message
    {

        public MessageFrom MessageFrom { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; }

        public Message() { }

    }

    public class MessageFrom
    {

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public MessageFrom()
        {

        }
    }

    public class Review
    {
        public Review() { }

        public int Stars { get; set; }
        public string Commentary { get; set; }
    }
}