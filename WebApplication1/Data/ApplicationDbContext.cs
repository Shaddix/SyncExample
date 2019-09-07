using WebApplication1.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using WebApplication1.Services;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {

        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientOnMainPage> PatientsOnMainPage { get; set; }

        public DbSet<DailyStatistic> DailyStatistics { get; set; }

        private readonly DataNotifier _dataNotifier;

        public ApplicationDbContext(
            DbContextOptions options,
            DataNotifier dataNotifier,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
            _dataNotifier = dataNotifier;
        }

        public override int SaveChanges()
        {
            var changes = GetChanges().Result;
            var result = base.SaveChanges();
            SendNotifications(changes).Wait();

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            var changes = await GetChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await SendNotifications(changes);

            return result;
        }

        public class EntityUpdateInfo
        {
            public string Type { get; set; }
            public int Id { get; set; }
            public BaseEntity Entity { get; internal set; }
        }

        protected async Task<List<EntityUpdateInfo>> GetChanges()
        {
            var result = new List<EntityUpdateInfo>();
            var entities = ChangeTracker
                .Entries()
                .Where(y => y.State != EntityState.Unchanged &&
                            y.State != EntityState.Detached)
                .ToList();

            foreach (var entityInfo in entities)
            {
                var baseEntity = entityInfo.Entity as BaseEntity;
                if (baseEntity == null)
                {
                    continue;
                }

                if (entityInfo.State == EntityState.Modified)
                {
                    result.Add(new EntityUpdateInfo()
                    {
                        Id = baseEntity.Id,
                        Type = baseEntity.GetType().Name.ToLower(),
                        Entity = baseEntity,
                    });
                }

                var originalEntity = entityInfo.OriginalValues.ToObject();

                var foreignKeys = entityInfo.Navigations
                    .Where(x => !x.Metadata.IsCollection())
                    .Select(x => new
                {
                    Name = x.Metadata.Name,
                    ForeignKey = x.Metadata.ForeignKey.Properties[0]
                }).Select(x => new
                {
                    Type = x.Name,
                    NewId = (int)x.ForeignKey.GetGetter().GetClrValue(baseEntity),
                    OldId = (int)x.ForeignKey.GetGetter().GetClrValue(originalEntity),
                }).ToList();

                foreach (var key in foreignKeys)
                {
                    result.Add(new EntityUpdateInfo()
                    {
                        Id = key.NewId,
                        Type = key.Type.ToLower(),
                        Entity = null,
                    });
                    if (key.NewId != key.OldId)
                    {
                        result.Add(new EntityUpdateInfo()
                        {
                            Id = key.OldId,
                            Type = key.Type.ToLower(),
                            Entity = null,
                        });
                    }
                }
            }

            return result;
        }


        protected async Task SaveChangesCommon2()
        {
            var entities = ChangeTracker
                .Entries()
                .Where(y => y.State != EntityState.Unchanged &&
                            y.State != EntityState.Detached)
                .ToList();

            foreach (var entityInfo in entities)
            {
                var baseEntity = entityInfo.Entity as BaseEntity;

                if (entityInfo.State == EntityState.Modified)
                {
                    await _dataNotifier.EntityUpdated(baseEntity.GetType().Name.ToLower(), baseEntity.Id);
                }
            }
        }

        private async Task SendNotifications(List<EntityUpdateInfo> updateInfo)
        {
            foreach (var item in updateInfo)
            {
                await _dataNotifier.EntityUpdated(item.Type, item.Id);
            }
        }
    }
}