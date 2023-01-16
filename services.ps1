#function DaprRun {
#    param (
#        $AppId,
#        $AppPort,
#        $DaprHttpPort,
#        $DaprGrpcPort
#    )
#    
#    dapr run --components-path ./dapr/components --app-id $AppId --app-port $AppPort --dapr-http-port $DaprHttpPort --dapr-grpc-port $DaprGrpcPort
#    Start-Sleep -Seconds 1
#}
#
#DaprRun "MyActorService" 5158 1501 54201
#DaprRun "MyActorClient"  5114 1500 54200
#
#Wait-Process