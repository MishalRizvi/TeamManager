using System;
using Microsoft.ML.Data;

namespace WebAppsNoAuth.Models
{
	public class ModelInput2
	{
		public ModelInput2()
		{
		}

		//public int UserId { get; set; }
		//public int ProjectId { get; set; }
		//public string CommentStr { get; set; }
		//public DateTime CommentDate { get; set; }


        [LoadColumn(0)]
        [ColumnName(@"id")]
        public float Id { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"person_name")]
        public string Person_name { get; set; }

        [LoadColumn(2)]
        [ColumnName(@"nine_box_category")]
        public string Nine_box_category { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"feedback")]
        public string Feedback { get; set; }

        [LoadColumn(4)]
        [ColumnName(@"adjusted")]
        public string Adjusted { get; set; }

        [LoadColumn(5)]
        [ColumnName(@"reviewed")]
        public string Reviewed { get; set; }

        [LoadColumn(6)]
        [ColumnName(@"label")]
        public string Label { get; set; }

        [LoadColumn(7)]
        [ColumnName(@"feedback_len")]
        public string Feedback_len { get; set; }

        [LoadColumn(8)]
        [ColumnName(@"num_of_sent")]
        public string Num_of_sent { get; set; }

        [LoadColumn(9)]
        [ColumnName(@"performance_class")]
        public string Performance_class { get; set; }

        [LoadColumn(10)]
        [ColumnName(@"potential_class")]
        public float Potential_class { get; set; }

        [LoadColumn(11)]
        [ColumnName(@"data_type")]
        public float Data_type { get; set; }
    }
}

