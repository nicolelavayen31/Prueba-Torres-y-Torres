import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CustomerOrder } from '../models/api.models';
import { BusinessApiService } from './business-api.service';

describe('BusinessApiService', () => {
  let service: BusinessApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BusinessApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends replacement order details with PUT and maps the API envelope', () => {
    const payload = {
      customer_id: 8,
      notes: 'Updated details',
      items: [{ description: 'Revised item', quantity: 3, unit_price: 12.5 }],
    };
    const responseOrder = {
      id: 19,
      customer_id: 8,
      status: 'Pending' as const,
      total: 37.5,
      notes: payload.notes,
      created_at: '2026-09-29T00:00:00Z',
      updated_at: '2026-09-29T00:00:00Z',
      items: [{ id: 3, description: 'Revised item', quantity: 3, unit_price: 12.5 }],
    } satisfies CustomerOrder;

    service.updateOrder(19, payload).subscribe((order) => {
      expect(order).toEqual(responseOrder);
    });

    const request = http.expectOne(`${environment.businessApiUrl}/orders/19`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush({ success: true, data: responseOrder, message: '', errors: [] });
  });

  it('requests daily activity from the matching dashboard route', () => {
    service.activityByDay().subscribe((points) => {
      expect(points).toEqual([{ date: '2026-09-29', count: 2, total: 25 }]);
    });

    const request = http.expectOne(`${environment.businessApiUrl}/dashboard/orders-by-day`);
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [{ date: '2026-09-29', count: 2, total: 25 }], message: '', errors: [] });
  });
});