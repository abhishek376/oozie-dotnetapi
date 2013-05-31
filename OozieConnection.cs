using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace Connections
{
	public class OozieConnection
	{
		public OozieConnection(string defaultHost = null, int defaultPort = 9200)
		{
			DefaultHost = defaultHost;
			DefaultPort = defaultPort;
		}


		public string DefaultHost { get; set; }

		public int DefaultPort { get; set; }

		public ICredentials Credentials { get; set; }


		public OperationResult Get(string command, string xmlData = null)
		{
			return ExecuteRequest("GET", command, null);
		}

		public OperationResult Post(string command, string xmlData = null)
		{
			return ExecuteRequest("POST", command, xmlData);
		}

		public OperationResult Put(string command, string xmlData = null)
		{
			return ExecuteRequest("PUT", command, xmlData);
		}

		public OperationResult Delete(string command, string xmlData = null)
		{
			return ExecuteRequest("DELETE", command, xmlData);
		}

		public OperationResult Head(string command, string xmlData = null)
		{
			return ExecuteRequest("HEAD", command, xmlData);
		}


		private OperationResult ExecuteRequest(string method, string command, string xmlData)
		{
			try
			{
				string uri = CommandToUri(command);
				var request = CreateRequest(method, uri);

				// Add request payload if any.
				if (!xmlData.IsNullOrEmpty())
				{
					byte[] buffer = Encoding.UTF8.GetBytes(xmlData);
					request.ContentLength = buffer.Length;
					using (Stream requestStream = request.GetRequestStream())
					{
						requestStream.Write(buffer, 0, buffer.Length);
					}
				}

				// Execute request.
				using (WebResponse response = request.GetResponse())
				{
					var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
					return new OperationResult(result);
				}

			}
			catch (WebException ex)
			{
				var message = ex.Message;
				var response = ex.Response;
				if (response != null)
				{
					using (var responseStream = response.GetResponseStream())
					{
						message = new StreamReader(responseStream, true).ReadToEnd();
					}
				}

				int statusCode = 0;
				if (response is HttpWebResponse)
					statusCode = (int)((HttpWebResponse)response).StatusCode;

				throw new OperationException(message, statusCode, ex);
			}

		}

		protected virtual HttpWebRequest CreateRequest(string method, string uri)
		{
			var request = (HttpWebRequest)WebRequest.Create(uri);

			request.Accept = "application/xml";
			request.ContentType = "application/xml";
			request.Timeout = 60 * 1000;
			request.ReadWriteTimeout = 60 * 1000;
			request.Method = method;
			request.Proxy = null;

			if (Credentials != null)
				request.Credentials = Credentials;

			return request;
		}


		private string CommandToUri(string command)
		{
			if (Uri.IsWellFormedUriString(command, UriKind.Absolute))
				return command;

			return @"http://{0}:{1}/{2}".F(DefaultHost, DefaultPort, command);
		}
	}
}
