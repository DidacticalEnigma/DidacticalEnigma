using System;

namespace DidacticalEnigma.Models
{
    public class DataSourceDescriptor
    {
        public Guid Guid { get; }

        public string Name { get; }

        public string AcknowledgementText { get; }

        // can be null
        public Uri DataSourceUrl { get; }

        public DataSourceDescriptor(Guid guid, string name, string acknowledgementText, Uri dataSourceUrl)
        {
            Guid = guid;
            Name = name;
            AcknowledgementText = acknowledgementText;
            DataSourceUrl = dataSourceUrl;
        }
    }
}