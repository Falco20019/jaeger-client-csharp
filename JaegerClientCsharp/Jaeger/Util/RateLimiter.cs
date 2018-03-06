using System;

namespace Jaeger.Util
{
    public class RateLimiter
    {
        private readonly double creditsPerNanosecond;
        private readonly double maxBalance;
        private double balance;
        private long lastTick;
        
        public RateLimiter(double creditsPerSecond, double maxBalance)
        {
            this.balance = maxBalance;
            this.maxBalance = maxBalance;
            this.creditsPerNanosecond = creditsPerSecond / 1.0e9;
        }

        public bool CheckCredit(double itemCost)
        {
            long currentTime = DateTime.Now.Ticks;
            double elapsedTime = currentTime - lastTick;
            lastTick = currentTime;
            balance += elapsedTime * creditsPerNanosecond;
            if (balance > maxBalance)
            {
                balance = maxBalance;
            }
            if (balance >= itemCost)
            {
                balance -= itemCost;
                return true;
            }
            return false;
        }
    }
}
