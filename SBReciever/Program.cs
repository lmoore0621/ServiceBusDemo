using Microsoft.Azure.ServiceBus;
using SBShared.Models;
using System.Text;
using System.Text.Json;

namespace SBReciever
{
	class Program
	{
		const string connectionString = "YourConnectionString";
		const string queueName = "YourQueue";
		static IQueueClient queueClient;

		static async Task Main(string[] args)
		{
			queueClient = new QueueClient(connectionString, queueName);

			var messageHandlerOptions = new MessageHandlerOptions(ExceptionRecievedHandler)
			{
				MaxConcurrentCalls = 1,

				//will not mark the message as complete
				AutoComplete = false
			};

			queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
			Console.ReadLine();
			await queueClient.CloseAsync();
		}

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
			var jsonString = Encoding.UTF8.GetString(message.Body);
			PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);
			Console.WriteLine($"Person recieved: { person.FirstName } { person.LastName }");

			await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ExceptionRecievedHandler(ExceptionReceivedEventArgs arg)
        {
			Console.WriteLine($"Message handler exception: {arg.Exception}");
			return Task.CompletedTask;
        }
    }
}