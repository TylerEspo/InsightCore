# InsightCore

<p align="center">
  <img src="https://i.gyazo.com/ac5af4a98490076982502bba2f9a6275.png" width="33%" />
  <img src="https://i.gyazo.com/09600cf2e1a06b43ebc4f0ba016baac4.png" width="33%" />
  <img src="https://i.gyazo.com/ff087c83ba3b1326bd37ce595f2174fa.png" width="33%" />
</p>


## Overview
InsightCore is an open-source ASP.NET Core MVC application designed for **W3C log analysis, anomaly detection, and real-time monitoring**. Built on **.NET 8**, it leverages **ML.NET** for detecting anomalies in logs and health metrics, providing valuable insights into server performance, security, and operational health.

With an intuitive web-based UI, InsightCore allows developers and administrators to **query, filter, and visualize** IIS logs effortlessly. It features a **built-in query language** for flexible log searching and analysis, inspired by industry-standard log query syntaxes.

## Key Features
- **W3C Log Parsing:** Automatically parses IIS logs into a structured format for analysis.
- **ML-Powered Anomaly Detection:** Uses **ML.NET** to identify unusual patterns in logs and health metrics.
- **Custom Query Language:** Execute powerful queries to analyze logs efficiently.
- **Real-Time Monitoring:** Detect and visualize server health status, request patterns, and anomalies.
- **Fast Filtering & Sorting:** Query logs instantly with commands like:
  ```
  index=iis ping | order desc | take 10
  ```
- **Interactive Dashboard:** Gain insights from **visualized metrics, error trends, and server health indicators**.
- **Extensible & Open Source:** Designed to be customized and integrated into any .NET environment.

## Getting Started
### Prerequisites
- .NET 8 SDK
- IIS with W3C logging enabled

## Query Language
InsightCore includes a query language for log analysis. Some examples:
- **Isolate to specific Uris**
  ```
  index=iis cs-uri-stem=/app/v1/ping
  ```
- **Get the last 10 requests using pipe keywords**
  ```
  index=iis cs-uri-stem=/app/v1/ping | order desc | take 10
  ```
- **Loose definition detection**
  ```
  index=iis /app/v1/ping myusername 
  ```
- **Strict definition detection**
  ```
  index=iis cs-uri-stem=/app/v1/ping cs-username=myusername 
  ```
- **Conditional Statements**
  ```
  index=iis cs-uri-stem=/app/v1/ping NOT cs-username=myusername  NOT testing
  ```
## Contributing
Contributions are welcome! Feel free to submit issues, feature requests, or pull requests.

## License
MIT License. See `LICENSE` for details.

---

**InsightCore** – Lightweight, fast, and intelligent log analysis for IIS.

