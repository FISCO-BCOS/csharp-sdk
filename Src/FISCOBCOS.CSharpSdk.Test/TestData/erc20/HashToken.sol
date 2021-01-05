pragma solidity >=0.4.24 <0.6.11;
import "./ERC20.sol";


contract HashToken is ERC20 {

  string public constant name = "HashToken";
  string public constant symbol = "HTC";
  uint8 public constant decimals = 18;

  uint256 public constant INITIAL_SUPPLY = 10000 * (1 ** uint256(decimals));

  /**
   * @dev Constructor that gives msg.sender all of existing tokens.
   */
  constructor() public {
    _mint(msg.sender, INITIAL_SUPPLY);
  }

}