using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models.Types;
using PizzaPlace.Models;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

    [TestClass]
    public class RestockingControllerTests
    {
        private static RestockingController GetController(Mock<IStockService> stockService) =>
        new(stockService.Object);

        [TestMethod]
        public async Task RestockAddsStock()
    {
        // Arrange
        var stock = new ComparableList<StockDto>();
        var resstock = new ComparableList<StockDto>();
        var stock1 = new StockDto(StockType.UnicornDust, 1);
        var stock2 = new StockDto(StockType.Bacon, 2);
        var stock3 = new StockDto(StockType.Chocolate, 11);
        var resstock1 = new StockDto(StockType.UnicornDust, 1);
        var resstock2 = new StockDto(StockType.Bacon, 12);
        var resstock3 = new StockDto(StockType.Chocolate, 16);
        stock.Add(stock1);
        stock.Add(stock2);
        stock.Add(stock3);
        resstock.Add(resstock1);
        resstock.Add(resstock2);
        resstock.Add(resstock3);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        stockService.Setup(x => x.Restock(stock))
            .ReturnsAsync(resstock);

        var controller = GetController(stockService);

        // Act
        var actual = await controller.Restock(stock);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(actual);
        stockService.VerifyAll();
    }

    }

