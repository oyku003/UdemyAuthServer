using Microsoft.EntityFrameworkCore;
using UdemyAuthServer.Core.UnitOfWork;

namespace UdemyAuthServer.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext dbContext;
        public UnitOfWork(AppDbContext appDbContext)
        {
                dbContext = appDbContext;
        }
        public void Commit()
        {
            dbContext.SaveChanges();
        }

        public async Task CommitAsync()
        {
           await dbContext.SaveChangesAsync();
        }
    }
}
