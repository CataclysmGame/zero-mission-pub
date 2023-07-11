mergeInto(LibraryManager.library, {
	Mint: function (skinId) {
		window._unityMintNFT(skinId)
	},

	CanMint: function() {
		return window._unityMintNFT ? true : false
	},
	
	GetAddress: function() {
		var addrStr = window._unityGetAddress()
		var bufferSize = lengthBytesUTF8(addrStr) + 1
		var buffer = _malloc(bufferSize)
        stringToUTF8(addrStr, buffer, bufferSize)
        return buffer
	},
})
