import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'customers',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/customer/customer-list/customer-list.component')
          .then(m => m.CustomerListComponent)
      },
      {
        path: 'new',
        loadComponent: () => import('./features/customer/customer-detail/customer-detail.component')
          .then(m => m.CustomerDetailComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/customer/customer-detail/customer-detail.component')
          .then(m => m.CustomerDetailComponent)
      }
    ]
  },  {
    path: 'products',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/product/product-list/product-list.component')
          .then(m => m.ProductListComponent)
      },
      {
        path: 'new',
        loadComponent: () => import('./features/product/product-detail/product-detail.component')
          .then(m => m.ProductDetailComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/product/product-detail/product-detail.component')
          .then(m => m.ProductDetailComponent)
      }
    ]
  },

  { path: '', redirectTo: 'customers', pathMatch: 'full' },
  { path: '**', redirectTo: 'customers' }
];
