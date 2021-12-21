The folder <Resources> contains resources that are used for:
- Non rigid face tracking 
	=>  			<models-68-BS.bin>, <face-model.obj>
	=>  			<models-68-robust-BS.bin>, <face-model.obj>
	=>  			<models-51.bin>, <face-model_internal.obj>

- Background/Body Segmentation (Drop) 
	iOS => 			<ENET100-192x128-32-BODY-ONLY.bytes>
	iOS (Compressed) => 	<ENET100-192x192-16-BODY.bytes>
	iOS (Robust) => 	<ENET150-192x192-16-BODY.bytes>
	Android => 		<body_192x192.bytes, body_192x192_q.bytes>
	Desktop (Landscape) =>	<seg-176x256-opt.bytes>
	Desktop (Portrait) =>	<seg-256x176-opt.bytes>

- Hair Detection/Segmentation:
	iOS => 			<ENET100-192x192-16-HAIRS-ONLY.bytes>
	Desktop (Landscape) => 	<hair-128x176T.bytes>
	Desktop (Portrait) => 	<hair-176x128T.bytes>
	Android => 		<hair_192x192.bytes, hair_192x192_q.bytes>

- Emotions Detection:
	iOS => 			<ENET50-16-EMO.bytes>
	Android => 		<ENET50-32-EMO-CPU.bytes>

To reduce App. footprint, you can remove unused resources when deploying your App.