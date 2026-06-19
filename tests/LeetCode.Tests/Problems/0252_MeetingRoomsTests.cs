using LeetCode.Problems;

namespace LeetCode.Tests.Problems;

public class MeetingRoomsTests
{
  public static TheoryData<int[][], bool> MeetingRoomsData => new()
  {
    { new int[][] { [0, 30], [5, 10], [15, 20] }, false },
    { new int[][] { [5, 8], [9, 15] }, true },
    { new int[][] { [5, 8], [8, 10] }, true },  // touching endpoints don't conflict
    { new int[][] { [5, 10] }, true },           // single meeting
    { Array.Empty<int[]>(), true },              // no meetings
  };

  [Theory]
  [MemberData(nameof(MeetingRoomsData))]
  public void Solve_MeetingRoomsConflicts(int[][] intervals, bool expected)
  {
    MeetingRooms meetingRooms = new();
    Assert.Equal(expected, meetingRooms.CanAttendMeetings(intervals));
  }
}
