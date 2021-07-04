using System;
using System.Linq;
using System.Threading;
using CardPool.Core;
using NUnit.Framework;

namespace CardPool.Tests
{
    public class CardsDrawerTests
    {
        private CardsPool _pool;
        [SetUp]
        public void Setup()
        {
            var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar,
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            _pool = new CardsPool(oneStarCards);
            _pool.BuildPool();
            Console.WriteLine(_pool.GetPoolProbabilityInfo());
            Console.WriteLine("sum of all probability: "+_pool.Cards.Sum(c=>c.RealProbability));
        }
        [Test]
        public void ProbabilityTest()
        {
           
        }
    }
}