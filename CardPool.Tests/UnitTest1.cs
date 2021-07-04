using System;
using System.Collections.Generic;
using System.Linq;
using CardPool.Core;
using NUnit.Framework;

namespace CardPool.Tests
{
    public class Tests
    {
        private CardsPool _pool;
        [SetUp]
        public void Setup()
        {
            var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5, 6, 7);
            _pool = new CardsPool(oneStarCards);
            _pool.SetPoolRarityProbability(CardRarity.OneStar, 0.1);
            _pool.BuildPool();
        }
        [Test]
        public void PrintTest1()
        {
            Console.WriteLine(_pool.GetPoolProbabilityInfo());
            Console.WriteLine("sum of all probability: "+_pool.Cards.Sum(c=>c.RealProbability));
        }
    
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}