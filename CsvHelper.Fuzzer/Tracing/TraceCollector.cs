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

	// here I use Anna Chao's estimator
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
			estimation = sN + ((f1 * (f1 - 1)) / 2);
		}

		return estimation;
	}

	// Here is Anna Chao's NPlusN estimator
	private double GetNPlusNEstimation()
	{
		double n = myTraces.Select(it => it.Value).Sum();
		double sN = myTraces.Count;
		double estimation = GetEstimation();
		double f1 = myTraces.Count(it => it.Value == 1);
		double f0_est = estimation - sN;
		double sNPlusN_est = sN + f0_est * (1 - Math.Pow(1 - f1 / (n * f0_est + f1), n));
		return sNPlusN_est;
	}

	//
	public bool ShouldRepeat()
	{
		if (myTraces.Select(it => it.Value).Sum() < 10)
		{
			// get initial 10 traces
			return true;
		}

		double sN = myTraces.Count;
		double sNPlusN_est = GetNPlusNEstimation();
		// difference less than 5%
		return sNPlusN_est - sN > 0.01 * sN;
	}

	public string GetStatistics()
	{
		var sb = new StringBuilder();
		var numberOfAttempts = myTraces.Select(it => it.Value).Sum();
		var numberOfUniqueTraces = myTraces.Count;
		var estimatedNumberOfTraces = GetEstimation();
		sb.Append($"After {numberOfAttempts} attempts, \n");
		sb.Append($"{numberOfUniqueTraces} unique traces found, \n");
		sb.Append($"{GetEstimation()} traces estimated.\n");
		sb.Append($"{GetNPlusNEstimation()} traces estimated in the next {numberOfAttempts} tries.\n");

		return sb.ToString();
	}
}
