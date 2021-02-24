using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System;
using System.Collections.Generic;

namespace HotChocolateService
{ 
   

    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(a => a.Hello()).Type<StringType>();
            descriptor.Field(a => a.GetMessageFrom()).Type<MessageFromType>();
           
        }
    }

    public class MsgType : ObjectType<Message>
    {
        protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
        {
            descriptor.Field(a => a.Content).Type<StringType>();
            descriptor.Field(a => a.SentAt).Type<DateType>();
            descriptor.Field(a => a.MessageFrom).Type<MessageFromType>();
          //  descriptor.Field<BookResolver>(t => t.GetBooks(default, default));
        }
    }

    public class MessageFromType : ObjectType<MessageFrom>
    {
        protected override void Configure(IObjectTypeDescriptor<MessageFrom> descriptor)
        {
            descriptor.Field(a => a.Id).Type<StringType>();
            descriptor.Field(a => a.DisplayName).Type<StringType>();           
        }
    }

    public class BookResolver
    {
        //  private readonly IBookService _bookService;

        /*  public BookResolver([Service]IBookService bookService)
          {
              _bookService = bookService;
          }
          public IEnumerable<Book> GetBooks(Author author, IResolverContext ctx)
          {
              return _bookService.GetAll().Where(b => b.AuthorId == author.Id);
          }*/

        public IEnumerable<Message> GetBooks(Message author, IResolverContext ctx)
        {
            return null;
        }
   }

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