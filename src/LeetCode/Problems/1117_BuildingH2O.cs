namespace LeetCode.Problems;

// https://leetcode.com/problems/building-h2o
public class H2O : IDisposable
{
  private int hCount;
  private readonly SemaphoreSlim hSem = new(1, 1);
  private readonly SemaphoreSlim oSem = new(0, 1);

  public void Hydrogen(Action releaseHydrogen)
  {
    hSem.Wait();
    // releaseHydrogen() outputs "H". Do not change or remove this line.
    releaseHydrogen();
    if (Interlocked.Increment(ref hCount) % 2 != 0)
      hSem.Release();
    else
      oSem.Release();
  }

  public void Oxygen(Action releaseOxygen)
  {
    oSem.Wait();
    // releaseOxygen() outputs "O". Do not change or remove this line.
    releaseOxygen();
    hSem.Release();
  }

  public void Dispose()
  {
    hSem.Dispose();
    oSem.Dispose();
    GC.SuppressFinalize(this);
  }
}
