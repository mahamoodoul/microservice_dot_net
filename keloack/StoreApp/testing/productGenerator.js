// productGenerator.js
// A standalone module to generate random OrderInfo payloads
// productGenerator.js
// A standalone module to generate random OrderRequest payloads
export function randomOrderRequest() {
    // sample product catalog
    const products = [
      'Widget',
      'Gadget',
      'Doohickey',
      'Thingamabob',
      'Whatchamacallit',
    ];
    const productName = products[Math.floor(Math.random() * products.length)];
    const productPrice = Math.floor(Math.random() * 100) + 1;


  
    return {
      ProductName:  productName,
      ProductPrice: productPrice,
    };
  }