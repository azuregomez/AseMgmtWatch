This project provides a solution to the problem of App Service Environment changing Azure Management IPs.  <br>
Management IPs are required when configuring User Defined Routes for Outbound Traffic Inspection. In order to have outbound traffic inspection with a default route sending traffic to a Firewall, the Route Table has to include all ASE Azure Management management IPS with nexthop Internet.
This solution includes 3 projects:
<ol>
<li> AseMgMtWatch. Azure Function on a 5 minute timer that invokes the ASE Management API: 
https://docs.microsoft.com/en-us/azure/app-service/environment/management-addresses#get-your-management-addresses-from-api<br>
The function will invoke the API and compare the IP list with a list that keeps in blob storage. If the list changed, the new list is saved in blob storage and an Azure Automation Webhook is invoked.  <br>
The Automation runbook powershell code will then update the ASE Subnet UDR with the new IPs.  The ps1 code is included in the AseMgMtWatch project under the Automation folder. You will need to update the runbook to provide your Route Table and RG name and also the IP of your firewall<br>
This function requires the following values as configuration:
<ul>
<li>Subcription ID
<li>ASE RG Name
<li>ASE Name
<li>AAD Tenant ID
<li>AAD Registsered App ID.  You need to register an App in AAD to invoke the Azure APIs and get an AAD token.
<li>AAD Registered App Key (password)
<li>Storage Account Name.  This is where the AF will store the existing ASE Management IPs as a json blob.
<li>Storage Account Key.
<li>Storage container name
<li>Automation Hook URL
</ul>
<li>AseApiAgent. This is a library where the majority of the functionality is implemented. There are 3 main components with corresponding interfaces so they can be tested: ASE Management API invocation, Blob Storage Persistence and Web Hook invocation.
<li>AseMgmtTest. Unit Tests
</ul>
