using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using VueCoreFramework.Data;
using VueCoreFramework.Extensions;
using VueCoreFramework.Models;
using VueCoreFramework.Services;

namespace VueCoreFramework.Test.Data
{
    [TestClass]
    public class RepositoryTest
    {
        private static ApplicationDbContext context;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseInMemoryDatabase();
            context = new ApplicationDbContext(optionsBuilder.Options);
        }

        [TestMethod]
        public async Task AddAsync_NoNavProp()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();

            Assert.IsNotNull(item);
        }

        [TestMethod]
        public async Task AddAsync_WithNavProp()
        {
            var repo = context.GetRepositoryForType(typeof(City));

            var childProp = typeof(City).GetProperty(nameof(City.Country));
            var navProp = typeof(City).GetProperty(nameof(City.CountryId));

            await repo.AddAsync(childProp, Guid.Empty.ToString());
            var item = context.Cities.FirstOrDefault();

            Assert.IsNotNull(item);
            Assert.AreEqual(Guid.Empty, navProp.GetValue(item));
        }

        [TestMethod]
        public async Task AddChildrenToCollectionAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(Airline));

            var childProp = typeof(Country).GetProperty(nameof(Country.Airlines));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();
            Assert.IsNotNull(parent);

            await childRepo.AddAsync(null, null);
            var child = context.Airlines.FirstOrDefault();
            Assert.IsNotNull(child);

            await parentRepo.AddChildrenToCollectionAsync(parent.Id.ToString(), childProp, new string[] { child.Id.ToString() });

            Assert.AreEqual(1, parent.Airlines.Count);
            Assert.AreEqual(parent.Id, parent.Airlines.FirstOrDefault().CountryId);
            Assert.AreEqual(child.Id, parent.Airlines.FirstOrDefault().AirlineId);
        }

        [TestMethod]
        public async Task FindAsync_ItemPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();

            Assert.IsNotNull(item);

            var vm = await repo.FindAsync(item.Id.ToString());
            Assert.IsTrue(vm.Keys.Contains(nameof(DataItem.Id).ToInitialLower()));
            Assert.AreEqual(item.Id.ToString(), vm[nameof(DataItem.Id).ToInitialLower()]);
        }

        [TestMethod]
        public async Task FindAsync_ItemNotPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            var vm = await repo.FindAsync(Guid.Empty.ToString());
            Assert.IsTrue(vm.Keys.Contains(nameof(DataItem.Id).ToInitialLower()));
            Assert.IsNull(vm[nameof(DataItem.Id).ToInitialLower()]);
        }

        [TestMethod]
        public async Task FindItemAsync_ItemPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();

            Assert.IsNotNull(item);

            var found = await repo.FindItemAsync(item.Id.ToString());
            Assert.AreEqual(item, found);
        }

        [TestMethod]
        public async Task FindItemAsync_ItemNotPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            var item = await repo.FindItemAsync(Guid.Empty.ToString());
            Assert.IsNull(item);
        }

        [TestMethod]
        public async Task GetAll_ItemsPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            await repo.AddAsync(null, null);

            var count = context.Countries.Count();
            var vms = await repo.GetAllAsync();
            Assert.AreEqual(count, vms.Count());
        }

        [TestMethod]
        public async Task GetAll_NoItemsPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.RemoveRangeAsync(context.Countries.Select(c => c.Id.ToString()));
            var vms = await repo.GetAllAsync();
            Assert.AreEqual(0, vms.Count());
        }

        [TestMethod]
        public async Task GetChildIdAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(City));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();

            Assert.IsNotNull(parent);

            var parentProp = typeof(Country).GetProperty(nameof(Country.Capitol));
            var childProp = typeof(City).GetProperty(nameof(City.CountryCapitol));

            var vm = await childRepo.AddAsync(childProp, parent.Id.ToString());
            var childId = vm[nameof(DataItem.Id).ToInitialLower()];

            var id = await parentRepo.GetChildIdAsync(parent.Id.ToString(), parentProp);
            Assert.AreEqual(childId, id);
        }

        [TestMethod]
        public async Task GetChildTotalAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(City));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();

            Assert.IsNotNull(parent);

            var parentProp = typeof(Country).GetProperty(nameof(Country.Cities));
            var childProp = typeof(City).GetProperty(nameof(City.Country));

            await childRepo.AddAsync(childProp, parent.Id.ToString());

            var total = await parentRepo.GetChildTotalAsync(parent.Id.ToString(), parentProp);
            Assert.AreEqual(1, total);
        }

        [TestMethod]
        public void GetFieldDefinitionsTest()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            var defs = repo.FieldDefinitions;

            Assert.IsTrue(defs.Any(d => d.Model == nameof(DataItem.Id).ToInitialLower()));
        }

        [TestMethod]
        public async Task GetPage_ItemsPresent()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            await repo.AddAsync(null, null);
            var count = context.Countries.Count();

            var vms = await repo.GetPageAsync(null, null, false, 1, 5, new string[] { },
                new List<Claim> { new Claim(CustomClaimTypes.PermissionDataAll, CustomClaimTypes.PermissionAll) });
            Assert.AreEqual(count, vms.Count());
        }

        [TestMethod]
        public async Task GetPage_ItemsPresent_Unauthorized()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            await repo.AddAsync(null, null);

            var vms = await repo.GetPageAsync(null, null, false, 1, 5, new string[] { },
                new List<Claim> { });
            Assert.AreEqual(0, vms.Count());
        }

        [TestMethod]
        public async Task GetPage_ItemsPresent_PartialAuthorization()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();
            Assert.IsNotNull(item);

            await repo.AddAsync(null, null);

            var vms = await repo.GetPageAsync(null, null, false, 1, 5, new string[] { },
                new List<Claim> { new Claim(CustomClaimTypes.PermissionDataAll, $"{nameof(Country)}{{{item.Id}}}") });
            Assert.AreEqual(1, vms.Count());
        }

        [TestMethod]
        public async Task GetTotalAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            await repo.AddAsync(null, null);

            var count = context.Countries.Count();

            var total = await repo.GetTotalAsync();
            Assert.AreEqual(count, total);
        }

        [TestMethod]
        public async Task RemoveAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();

            Assert.IsNotNull(item);
            var count = context.Countries.Count();

            await repo.RemoveAsync(item.Id.ToString());

            var newCount = context.Countries.Count();
            Assert.AreEqual(count - 1, newCount);
        }

        [TestMethod]
        public async Task RemoveChildrenFromCollectionAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(Airline));

            var childProp = typeof(Country).GetProperty(nameof(Country.Airlines));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();
            Assert.IsNotNull(parent);

            await childRepo.AddAsync(null, null);
            var child = context.Airlines.FirstOrDefault();
            Assert.IsNotNull(child);

            await parentRepo.AddChildrenToCollectionAsync(parent.Id.ToString(), childProp, new string[] { child.Id.ToString() });

            Assert.AreEqual(1, parent.Airlines.Count);

            await parentRepo.RemoveChildrenFromCollectionAsync(parent.Id.ToString(), childProp, new string[] { child.Id.ToString() });

            Assert.AreEqual(0, parent.Airlines.Count);
        }

        [TestMethod]
        public async Task RemoveFromParentAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(City));

            var childProp = typeof(City).GetProperty(nameof(City.Country));

            await repo.AddAsync(childProp, Guid.Empty.ToString());
            var item = context.Cities.FirstOrDefault();
            var count = context.Cities.Count();

            Assert.IsNotNull(item);

            var removed = await repo.RemoveFromParentAsync(item.Id.ToString(), childProp);
            var newCount = context.Cities.Count();
            Assert.IsTrue(removed);
            Assert.AreEqual(count - 1, newCount);
        }

        [TestMethod]
        public async Task RemoveRangeAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            await repo.AddAsync(null, null);

            await repo.RemoveRangeAsync(context.Countries.Select(c => c.Id.ToString()));

            Assert.AreEqual(0, context.Countries.Count());
        }

        [TestMethod]
        public async Task RemoveRangeFromParentAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(City));

            var childProp = typeof(City).GetProperty(nameof(City.Country));

            await repo.AddAsync(childProp, Guid.Empty.ToString());
            await repo.AddAsync(childProp, Guid.Empty.ToString());

            var count = context.Cities.Count();

            var ids = context.Cities.Select(c => c.Id.ToString()).ToList();
            var removedIds = await repo.RemoveRangeFromParentAsync(ids, childProp);
            var newCount = context.Cities.Count();
            Assert.AreEqual(count - removedIds.Count, newCount);
        }

        [TestMethod]
        public async Task ReplaceChildAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(City));

            var childProp = typeof(City).GetProperty(nameof(City.CountryCapitol));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();

            await childRepo.AddAsync(childProp, parent.Id.ToString());
            var oldChild = context.Cities.FirstOrDefault();
            await childRepo.AddAsync(null, null);
            var newChild = context.Cities.FirstOrDefault(c => c != oldChild);

            var count = context.Cities.Count();

            var oldId = await childRepo.ReplaceChildAsync(parent.Id.ToString(), newChild.Id.ToString(), childProp);
            var newCount = context.Cities.Count();
            Assert.IsNotNull(oldId);
            Assert.AreEqual(count - 1, newCount);
        }

        [TestMethod]
        public async Task ReplaceChildWithNewAsyncTest()
        {
            var parentRepo = context.GetRepositoryForType(typeof(Country));
            var childRepo = context.GetRepositoryForType(typeof(City));

            var childProp = typeof(City).GetProperty(nameof(City.CountryCapitol));

            await parentRepo.AddAsync(null, null);
            var parent = context.Countries.FirstOrDefault();

            await childRepo.AddAsync(childProp, parent.Id.ToString());
            var oldChild = context.Cities.FirstOrDefault();

            var count = context.Cities.Count();

            var (vm, oldId) = await childRepo.ReplaceChildWithNewAsync(parent.Id.ToString(), childProp);
            var newCount = context.Cities.Count();
            Assert.IsNotNull(oldId);
            Assert.AreEqual(count, newCount);
        }

        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            var repo = context.GetRepositoryForType(typeof(Country));

            await repo.AddAsync(null, null);
            var item = context.Countries.FirstOrDefault();

            Assert.IsNotNull(item);

            await repo.UpdateAsync(item);
        }
    }
}
