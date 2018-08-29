using System;

namespace DidacticalEnigma.Models
{
    public class DataSourceDescriptor
    {
        public string Name { get; }

        public string AcknowledgementText { get; }

        // can be null
        public Uri DataSourceUrl { get; }

        public DataSourceDescriptor(string name, string acknowledgementText, Uri dataSourceUrl)
        {
            Name = name;
            AcknowledgementText = acknowledgementText;
            DataSourceUrl = dataSourceUrl;
        }
    }
}