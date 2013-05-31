oozie-dotnetapi
===============

Simple Oozie .NET API

Calling the Oozie workflow from C# using the Oozie REST API. 

For more detailed information http://abhishek376.wordpress.com/2013/05/31/oozie-workflow-net-api/

		public void StartOozieWorkflow(string databaseName, string tableName)
		{
			var connection = new OozieConnection("hadoop1", 11000);
			const string xmlData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><configuration><property> <name>mapred.job.queue.name</name> <value>default</value> </property> <property> <name>user.name</name> <value>root</value> </property> <property> <name>oozie.use.system.libpath</name> <value>true</value> </property> <property> <name>tableName</name> <value>{0}</value> </property> <property> <name>databaseName</name> <value>{1}</value> </property> <property> <name>oozie.wf.application.path</name> <value>hdfs://hadoop1.allegiance.local:8020/user/root/hiveoozie</value> </property> </configuration>";
			var serializer = new JsonNetSerializer();
			var result = connection.Post("oozie/v1/jobs?action=start", String.Format(xmlData, tableName, databaseName));

			var id = serializer.Deserialize<OozieNewJobResult>(result).id;
			//GET /oozie/v1/job/job-3?show=info
			var statusinfo = connection.Get("oozie/v1/job/" + id + "?show=info");
			var status = serializer.Deserialize<OozieJobStatus>(statusinfo).Status;

			while (status != "SUCCEEDED")
			{
				//TODO : Record the status in a shared dictonary for web UI to poll the status
				statusinfo = connection.Get("oozie/v1/job/" + id + "?show=info");
				status = serializer.Deserialize<OozieJobStatus>(statusinfo).Status;
				Thread.Sleep(3000);
			}
		}