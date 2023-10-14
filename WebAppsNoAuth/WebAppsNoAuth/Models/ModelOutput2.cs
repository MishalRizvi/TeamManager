using System;
using Microsoft.ML.Data;

namespace WebAppsNoAuth.Models
{
	public class ModelOutput2
	{
		public ModelOutput2()
		{
		}

		//public string Category { get; set; }
		//public float Probabilty { get; set; }

        [ColumnName(@"id")]
        public float Id { get; set; }

        [ColumnName(@"person_name")]
        public float[] Person_name { get; set; }

        [ColumnName(@"nine_box_category")]
        public uint Nine_box_category { get; set; }

        [ColumnName(@"feedback")]
        public float[] Feedback { get; set; }

        [ColumnName(@"adjusted")]
        public float[] Adjusted { get; set; }

        [ColumnName(@"reviewed")]
        public float[] Reviewed { get; set; }

        [ColumnName(@"label")]
        public float[] Label { get; set; }

        [ColumnName(@"feedback_len")]
        public float[] Feedback_len { get; set; }

        [ColumnName(@"num_of_sent")]
        public float[] Num_of_sent { get; set; }

        [ColumnName(@"performance_class")]
        public float[] Performance_class { get; set; }

        [ColumnName(@"potential_class")]
        public float Potential_class { get; set; }

        [ColumnName(@"data_type")]
        public float Data_type { get; set; }

        [ColumnName(@"Features")]
        public float[] Features { get; set; }

        [ColumnName(@"PredictedLabel")]
        public string PredictedLabel { get; set; }

        [ColumnName(@"Score")]
        public float[] Score { get; set; }

    }
}

