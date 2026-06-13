using System.Collections.Concurrent;
using LeetCode.Problems;

namespace Leetcode.Tests.Problems;

public class BuildingH2OTests
{
  [Theory]
  [InlineData("HOH")]
  [InlineData("OOHHHH")]
  [InlineData("HHHHHHHHHHOHHOHHHHOOHHHOOOOHHOOHOHHHHHOOHOHHHOOOOOOHHHHHHHHH")]
  public async Task Solve_BuildingH2OTestsAsync(string water)
  {
    using H2O sut = new();
    ConcurrentQueue<char> queue = [];
    List<Task> hydrogens = [];
    List<Task> oxygens = [];

    // "HOH" is 1 molecule (2H + 1O)
    int n = water.Length;
    int oxygenCount = water.Count(c => c == 'O');

    for (int i = 0; i < oxygenCount; i++)
    {
      hydrogens.Add(Task.Run(() => sut.Hydrogen(() => queue.Enqueue('H'))));
      hydrogens.Add(Task.Run(() => sut.Hydrogen(() => queue.Enqueue('H'))));
      oxygens.Add(Task.Run(() => sut.Oxygen(() => queue.Enqueue('O'))));
    }

    await Task.WhenAll([.. hydrogens, .. oxygens]);

    string outputString = string.Concat(queue);
    for (int i = 0; i < n; i += 3)
    {
      Assert.Equal(2, outputString.Skip(i).Take(3).Count(c => c == 'H'));
      Assert.Equal(1, outputString.Skip(i).Take(3).Count(c => c == 'O'));
    }
  }
}
