pragma solidity >=0.4.24 <0.6.11;
import "./ERC20.sol";
contract HashToken is ERC20 {

    string private _name;
    string private _symbol;
    uint8 public constant _decimals = 0;
   // uint256 public constant INITIAL_SUPPLY = 10000 * (10 ** uint256(_decimals));
uint256 public constant INITIAL_SUPPLY = 1000000;
   constructor(string memory name_, string memory symbol_) public {
        _name = name_;
        _symbol = symbol_;
        _mint(msg.sender,INITIAL_SUPPLY);
    }



  /**
     * @dev Returns the name of the token.
     */
    function name() public view returns (string memory) {
        return _name;
    }

    /**
     * @dev Returns the symbol of the token, usually a shorter version of the
     * name.
     */
    function symbol() public view returns (string memory) {
        return _symbol;
    }
  
    function decimals() public pure returns (uint8) {
        return _decimals;
    }

}