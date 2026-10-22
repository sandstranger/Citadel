using NUnit.Framework;

namespace Tests
{
    public class StringBuilderPoolTests
    {
        private readonly StringBuilderPool _pool = new();
        [Test]
        public void RentReturnsCleanBuilder()
        {
            using (var firstLease = _pool.Rent())
            {
                firstLease.Builder.Append("first");
                Assert.That(firstLease.Builder.ToString(), Is.EqualTo("first"));
            }

            using (var secondLease = StringBuilderPool.Rent())
            {
                Assert.That(secondLease.Builder.Length, Is.Zero);
            }
        }

        [Test]
        public void NestedLeasesUseDifferentBuilders()
        {
            using (var outerLease = StringBuilderPool.Rent())
            {
                var outerBuilder = outerLease.Builder;
                outerBuilder.Append("outer");

                using (var innerLease = StringBuilderPool.Rent())
                {
                    var innerBuilder = innerLease.Builder;

                    Assert.That(innerBuilder, Is.Not.SameAs(outerBuilder));

                    innerBuilder.Append("inner");
                    Assert.That(outerBuilder.ToString(), Is.EqualTo("outer"));
                    Assert.That(innerBuilder.ToString(), Is.EqualTo("inner"));
                }
            }
        }

        [Test]
        public void RentHonorsRequestedInitialCapacity()
        {
            const int requestedCapacity = 4096;

            using (var lease = StringBuilderPool.Rent(requestedCapacity))
            {
                Assert.That(lease.Builder.Capacity, Is.GreaterThanOrEqualTo(requestedCapacity));
            }
        }
    }
}
