using HotChocolate.Language;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotChocolateService
{
    public class RedisProcessor<TTopic, TSource> where TSource : class
    {
        private TSource Source;
        private TTopic Topic;
        private Thread receiveRedisThread;
     
        DocumentNode Document;
        IObservable<IFieldResolver> FieldStream;
       // MessageHandler<TSource> Handler;
       public RedisProcessor() 
        {
            receiveRedisThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveRedisThread.IsBackground = true;
            receiveRedisThread.Start();

        }

        private async void ReceiveMessage()
        {

            // while (true)
            // {
            //    ThreadPool.QueueUserWorkItem(
            //      state =>
            //     {
            try
            {
                while (true)
                {
                    var redisManager = new RedisManagerPool("redisserver-001.03brtg.0001.use1.cache.amazonaws.com");


                    var c = await redisManager.GetClientAsync();
                    //    await c.StoreAsync<Message>(msg);
                    var x = c.As<Message>();
                   
                    var m = await x.GetAllAsync();
                    Console.WriteLine($"Message {m[0].Content}");
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception");
            }
            //}
            // );

            /* if (_state != ServerState.ShuttingDown)
                 abort();*/
            //}
        }
    }
}
