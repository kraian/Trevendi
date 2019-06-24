using Google.Cloud.Datastore.V1;

namespace Infrastructure
{
    public abstract class AppDbContext
    {
        protected readonly DatastoreDb _db;

        public AppDbContext(string projectId)
        {
            _db = DatastoreDb.Create(projectId);
        }
    }
}
