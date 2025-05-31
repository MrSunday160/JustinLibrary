using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Justin.EntityFramework.Model {
    public class BaseDbContext<TContext>(DbContextOptions<TContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options) where TContext : DbContext {

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        #region Db Setup

        public override int SaveChanges(bool acceptAllChangesOnSuccess) {
            SetDefaultValues();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
            SetDefaultValues();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if(modelBuilder != null) {

                modelBuilder.UseIdentityByDefaultColumns();
                modelBuilder.HasPostgresExtension("citext");
                //modelBuilder.Entity<Base>().HasQueryFilter(x => !x.IsDeleted);
                base.OnModelCreating(modelBuilder);

            }
        }

        public virtual void SetDefaultValues() {

            var entities = ChangeTracker.Entries().Where(x => x.Entity is Base && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));

            string currUsername = "Anonymous";
            if(_httpContextAccessor.HttpContext != null) {
                // try get username
                var user = _httpContextAccessor.HttpContext.User;
                var userName = user.FindFirst(x => x.Type == "Name");

                if(userName != null)
                    currUsername = userName.Value;

            }

            SwitchState(entities, currUsername);

        }

        public virtual void SwitchState(IEnumerable<EntityEntry> entities, string currUsername) {

            foreach(var entity in entities) {
                switch(entity.State) {
                    case EntityState.Added:
                        ((Base)entity.Entity).CreatedDate = DateTime.Now;
                        ((Base)entity.Entity).IsDeleted = false;
                        ((Base)entity.Entity).CreatedBy = currUsername;
                        break;
                    case EntityState.Modified:
                        ((Base)entity.Entity).IsDeleted = false;
                        ((Base)entity.Entity).UpdatedDate = DateTime.Now;
                        ((Base)entity.Entity).UpdatedBy = currUsername;
                        break;
                    case EntityState.Deleted:
                        entity.State = EntityState.Modified;
                        ((Base)entity.Entity).IsDeleted = true;
                        break;
                }
            }

        }
        #endregion

    }
}
