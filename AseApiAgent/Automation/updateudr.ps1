    #param
    param(
        [Parameter (Mandatory = $true)]
        [object]$WebhookData        
    )
    Write-output "WebhookData" $WebhookData
     if ($WebhookData -ne $null) {
        $WebhookBody = $WebHookData.RequestBody
        $input = (ConvertFrom-Json -InputObject $WebhookBody)
        # parameters 
        $description = $input.description
        $endpoints = $input.endpoints           
        write-output $description
        #login to azure
        $connectionName = "AzureRunAsConnection"
        try
        {
            # Get the connection "AzureRunAsConnection "
            $servicePrincipalConnection=Get-AutomationConnection -Name $connectionName         

            "Logging in to Azure..."
            Add-AzureRmAccount `
                -ServicePrincipal `
                -TenantId $servicePrincipalConnection.TenantId `
                -ApplicationId $servicePrincipalConnection.ApplicationId `
                -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint 
        }
        catch {
            if (!$servicePrincipalConnection)
            {
                $ErrorMessage = "Connection $connectionName not found."
                throw $ErrorMessage
            } else{
                Write-Error -Message $_.Exception
                throw $_.Exception
            }
        }        
		# TODO: Change UDR Name and RG, or even better: parameterize it
		$udrname = "skokie-ase-Route-Table"
		$rgname = "skokiease-rg"
		# getting route table
		$udr = get-azroutetable -resourcegroupname $rgname -name $udrname
		# crealing routes
		$udr.Routes.Clear()
		$i = 1
		# adding routes 1 by 1
		foreach ($endpoint in $endpoints)
        {
            write-output $endpoint
			$name = "ase"+$i
			add-azrouteconfig -name $name -AddressPrefix $endpoint -NextHopType "Internet" -routetable $udr 		
			$i=$i+1
        }
		# adding final default route
		# TODO: Replace with Firewall IP as nexthop and NetHopType VirtualAppliance
        add-azrouteconfig -name "default" -AddressPrefix "0.0.0.0/0" -NextHopType "Internet" -routetable $udr
		set-azroutetable -routetable $udr
        write-output "Updated UDRs"
    }
     else {
        Write-Error "Runbook to be started only from webhook."
    }



