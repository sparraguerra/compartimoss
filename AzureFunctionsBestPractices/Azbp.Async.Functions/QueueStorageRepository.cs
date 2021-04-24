using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Azbp.Async.Functions
{
    public interface IQueueStorageRepository
    {
        Task CreateMessageAsync(string message);
    }

    public class QueueStorageRepository : IQueueStorageRepository
    {
        private readonly string connectionString;
        private readonly string queueName;

        public QueueStorageRepository(IConfiguration configuration)
        {
            this.connectionString = configuration["ConnectionStrings:QueueDemo"];
            this.queueName = configuration["AppSettings:QueueName"];
        }

        public async Task CreateMessageAsync(string message)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(queueName);

            await queue.CreateIfNotExistsAsync();

            await queue.AddMessageAsync(new CloudQueueMessage(message));
        }
    }
}
