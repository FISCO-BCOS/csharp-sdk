pragma solidity >=0.4.24 <0.6.11;
contract QTest {
 function test() public view returns(bytes memory){
        //一个参数对应一个32位的abi编码：0x00000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000000000002
        
        uint a = 1;
     
       return abi.encode(a);
        // encodePacked编码只能是变量【如果encodePacked(1,2)会报错！】
       // return abi.encodePacked(a,b);
        // 0xcccdda2c0000000000000000000000000000000000000000000000000000000000000001
        // 前面四位cccdda2c是函数set(uint)的签名（selector）,后面32位是参数1的ABI编码！
        //return abi.encodeWithSignature("set(uint)", 1);
    }


    
}