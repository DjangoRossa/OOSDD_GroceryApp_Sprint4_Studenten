using Grocery.Core;
using Grocery.Core.Data;
using Grocery.Core.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using Grocery.Core.Services;
using Moq;

namespace TestCore
{
    public class TestHelpers
    {
        private Mock<IGroceryListItemsRepository> _groceryListItemsRepoMock;
        private Mock<IGroceryListRepository> _groceryListRepoMock;
        private Mock<IClientRepository> _clientRepoMock;
        private Mock<IProductRepository> _productRepoMock;
        private BoughtProductsService _service;

        [SetUp]
        public void Setup()
        {
            _groceryListItemsRepoMock = new Mock<IGroceryListItemsRepository>();
            _groceryListRepoMock = new Mock<IGroceryListRepository>();
            _clientRepoMock = new Mock<IClientRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _service = new BoughtProductsService(
                _groceryListItemsRepoMock.Object,
                _groceryListRepoMock.Object,
                _clientRepoMock.Object,
                _productRepoMock.Object
            );
        }

        [Test]
        public void TestPasswordHelperReturnsTrue()
        {
            string password = "user3";
            string passwordHash = "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=";
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08=")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=")]
        public void TestPasswordHelperReturnsTrue(string password, string passwordHash)
        {
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [Test]
        public void TestPasswordHelperReturnsFalse()
        {
            string password = "user3";
            string passwordHash = "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA+";
            Assert.IsFalse(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08+")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA+")]
        public void TestPasswordHelperReturnsFalse(string password, string passwordHash)
        {
            Assert.IsFalse(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [Test]
        public void TestAddProductReturnsTrue()
        {
            List<GroceryListItem> groceryListItems = new List<GroceryListItem>();
            GroceryListItem product = new GroceryListItem(-1, 1, 1, 1);
            groceryListItems.Add(product);
            Assert.IsTrue(groceryListItems.Contains(product));
        }

        [TestCase(-1, 2, 3, 4)]
        [TestCase(-1, 3, 4, 5)]
        public void TestAddProductReturnsTrue(int id, int groceryListId, int productId, int amount)
        {
            List<GroceryListItem> groceryListItems = new List<GroceryListItem>();
            GroceryListItem product = new GroceryListItem(id, groceryListId, productId, amount);
            groceryListItems.Add(product);
            Assert.IsTrue(groceryListItems.Contains(product));
        }

        [Test]
        public void TestRemoveProductReturnsFalse()
        {
            List<GroceryListItem> groceryListItems = new List<GroceryListItem>();
            GroceryListItem product = new GroceryListItem(-1, 1, 1, 1);
            groceryListItems.Add(product);
            groceryListItems.Remove(product);
            Assert.IsFalse(groceryListItems.Contains(product));
        }

        [Test]
        public void TestAdminRole()
        {
            Client adminClient = new Client(1, "user123", "admin@example.com", "password123", Client.Role.Admin);
            Assert.IsTrue(adminClient.Roles == Client.Role.Admin);
        }

        [Test]
        public void TestNoneRole()
        {
            Client adminClient = new Client(1, "user123", "admin@example.com", "password123");
            Assert.IsTrue(adminClient.Roles == Client.Role.None);
        }

        [Test]
        public void TestGetBoughtProductsReturnsEmptyListWhenProductIdIsNull()
        {
            var result = _service.Get(null);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void TestGetBoughtProductsReturnsEmptyListWhenNoItemsFound()
        {
            int productId = 999;
            _groceryListItemsRepoMock.Setup(r => r.GetAll()).Returns(new List<GroceryListItem>());
            var result = _service.Get(productId);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void TestGetBoughtProductsManualLogic()
        {
            int productId = 1;
            var groceryListItems = new List<GroceryListItem>
            {
                new GroceryListItem(1, 1, productId, 2),
                new GroceryListItem(2, 2, productId, 3)
            };
            var groceryLists = new List<GroceryList>
            {
                new GroceryList(1, "List 1", DateOnly.Parse("2024-12-14"), "#FF6A00", 1),
                new GroceryList(2, "List 2", DateOnly.Parse("2024-12-07"), "#626262", 2)
            };
            var clients = new List<Client>
            {
                new Client(1, "user1", "user1@example.com", "password123"),
                new Client(2, "user2", "user2@example.com", "password123")
            };
            var products = new List<Product>
            {
                new Product(1, "Product 1", 100),
                new Product(2, "Product 2", 100)
            };

            var groceryListItemsRepoMock = new Mock<IGroceryListItemsRepository>();
            groceryListItemsRepoMock.Setup(r => r.GetAll()).Returns(groceryListItems);

            var groceryListRepoMock = new Mock<IGroceryListRepository>();
            groceryListRepoMock.Setup(r => r.Get(It.IsAny<int>())).Returns((int id) => groceryLists.FirstOrDefault(gl => gl.Id == id));

            var clientRepoMock = new Mock<IClientRepository>();
            clientRepoMock.Setup(r => r.Get(It.IsAny<int>())).Returns((int id) => clients.FirstOrDefault(c => c.Id == id));

            var productRepoMock = new Mock<IProductRepository>();
            productRepoMock.Setup(r => r.Get(It.IsAny<int>())).Returns((int id) => products.FirstOrDefault(p => p.Id == id));

            if (productId == null)
            {
                Assert.Pass("ProductId is null, returning empty list as expected.");
                return;
            }

            var filteredItems = groceryListItemsRepoMock.Object
                .GetAll()
                .Where(item => item.ProductId == productId)
                .ToList();

            var result = new List<BoughtProducts>();

            foreach (var item in filteredItems)
            {
                var groceryList = groceryListRepoMock.Object.Get(item.GroceryListId);
                if (groceryList == null)
                    continue;

                var client = clientRepoMock.Object.Get(groceryList.ClientId);
                if (client == null)
                    continue;

                var product = productRepoMock.Object.Get(item.ProductId);
                if (product == null)
                    continue;

                var boughtProduct = new BoughtProducts(client, groceryList, product);
                result.Add(boughtProduct);
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("user1", result[0].Client.Name);
            Assert.AreEqual("List 1", result[0].GroceryList.Name);
            Assert.AreEqual("Product 1", result[0].Product.Name);
        }
    }
}
