using System;
using System.IO;
using System.Threading;

namespace Metrics.RollingCsvReporter.Tests
{
  public class TemporaryDirectoryFixture : IDisposable
  {
    readonly string _directoryPath;

    public TemporaryDirectoryFixture()
    {
      string randomDirectoryName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

      _directoryPath = Path.Combine(Path.GetTempPath(), randomDirectoryName);

      Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath
    {
      get { return _directoryPath; }
    }

    public void Dispose()
    {
      try
      {
        if (Directory.Exists(_directoryPath))
          Directory.Delete(_directoryPath, true);
      }
      catch (IOException)
      {
        // Give other process a chance to release their handles
        // see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true/1703799#1703799
        Thread.Sleep(0);
        try
        {
          Directory.Delete(_directoryPath, true);
        }
        catch
        {
          var longDelayS = 2;
          try
          {
            // This time we'll have to be _really_ patient
            Thread.Sleep(TimeSpan.FromSeconds(longDelayS));
            Directory.Delete(_directoryPath, true);
          }
          catch (Exception ex)
          {
            throw new Exception(@"Could not delete " + GetType() + @" directory: """ + _directoryPath + @""" due to locking, even after " + longDelayS + " seconds", ex);
          }
        }
      }
    }
  }
}