namespace LeetCode.Problems;

// https://leetcode.com/problems/meeting-rooms/description/
public class MeetingRooms
{
  public bool CanAttendMeetings(int[][] intervals)
  {
    int[][] sortedIntervals = [.. intervals.OrderBy(i => i[0]).ThenBy(i => i[1])];

    for (int i = 1; i < sortedIntervals.Length; i++)
    {
      if (sortedIntervals[i][0] < sortedIntervals[i - 1][1])
      {
        return false;
      }
    }

    return true;
  }
}
