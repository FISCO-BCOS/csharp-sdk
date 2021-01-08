pragma solidity ^0.5.1;
contract AbiEncode {
 function abiEndode(uint a,string memory b,uint c) public view returns(bytes memory){
        //一个参数对应一个32位的abi编码：0x00000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000000000002
        
        uint a = 1;
     
       return abi.encode(a,b,c);
    }
 function abiDecode(bytes memory encodeData) public view returns(uint,string memory,uint ){
    
       (uint a,string memory b,uint c)= abi.decode(encodeData,(uint,string,uint));
       return (a,b,c);
    }

    
}