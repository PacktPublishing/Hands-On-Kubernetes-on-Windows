kubectl run `
        --generator=run-pod/v1 powershell-debug `
        -i --tty `
        --image=mcr.microsoft.com/powershell:nanoserver-1809 `
        --restart=Never `
        --overrides='{\"apiVersion\": \"v1\", \"spec\": {\"nodeSelector\": { \"beta.kubernetes.io/os\": \"windows\" }}}'