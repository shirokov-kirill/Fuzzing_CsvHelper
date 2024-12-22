using System.Text;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Fuzzer.Tracing;

public class TraceCollector(FuzzingLogsCollector logsCollector)
{
	private readonly Dictionary<int, int> myTraces = new Dictionary<int, int>();

	public void Next()
	{
		logsCollector.Reset();
	}

	public void Commit()
	{
		// build string and get hash code
		var trace = logsCollector
			.GetTrace()
			.ToList();
		trace.Sort();
		var sb = new StringBuilder();
		foreach (var record in trace)
		{
			sb.Append(record);
		}
		var hashCode = sb.ToString().GetHashCode();

		AddItem(hashCode);
	}

	private void AddItem(int hashCode)
	{
		if (!myTraces.ContainsKey(hashCode))
		{
			myTraces.Add(hashCode, 1);
		}
		else
		{
			myTraces[hashCode]++;
		}
	}

	// here I use Anna Chao's success estimator
	private double GetEstimation()
	{
		// number of singletons
		double f1 = myTraces.Count(it => it.Value == 1);
		// number of doubletons
		double f2 = myTraces.Count(it => it.Value == 2);
		// number of different traces found
		double sN = myTraces.Count;

		double estimation;
		if (f2 > 0)
		{
			estimation = sN + (f1 * f1) / (2 * f2);
		}
		else
		{
			estimation = sN + f1 * (f1 - 1) / 2;
		}

		return estimation;
	}

	public bool ShouldRepeat()
	{
		if (myTraces.Select(it => it.Value).Sum() < 100)
		{
			// get initial 100 traces
			return true;
		}

		var estimation = GetEstimation();

		// difference less than 5%
		return (estimation - myTraces.Count) * 20 > myTraces.Count;
	}

	public string GetStatistics()
	{
		var sb = new StringBuilder();
		var numberOfAttempts = myTraces.Select(it => it.Value).Sum();
		var numberOfUniqueTraces = myTraces.Count;
		var estimatedNumberOfTraces = GetEstimation();
		sb.Append($"After {numberOfAttempts} attempts, \n");
		sb.Append($"{numberOfUniqueTraces} unique traces found, \n");
		sb.Append($"{estimatedNumberOfTraces} traces estimated.\n");

		return sb.ToString();
	}
}
